var request = require('request')
var turf = require('turf');
var fs = require('fs');
var https = require('https');
const pg = require('pg');

var gm_key = 'put_your_gm_key_here';

const connectionString = process.env.DATABASE_URL || 'postgres://postgres:postgres@localhost:5432/postgres';
const client = new pg.Client(connectionString);
client.connect();

// workaround for socket error
// https.globalAgent.maxSockets = 1;

var brievenbussen = fs.readFileSync('../data/170608/brievenbussen.geojson');
var woonkernen = fs.readFileSync('../data/woonkernen.json');

brievenbussen = JSON.parse(brievenbussen);
woonkernen = JSON.parse(woonkernen);

console.log('Starting processor');

// clean table
var sql = "truncate bb_gebieden";
var query = client.query(sql);

var max_records = brievenbussen.features.length;
var current_record = 0;

// loop through all brievenbussen
//brievenbussen.features.forEach(function(brievenbus) {
for(var i=0;i<max_records;i++){
    brievenbus=brievenbussen.features[i];
    // console.log(i + ": brievenbus: " + brievenbus.properties.id);
    // if brievenbus is inside woonkern with >5000 inwoners
    // then radius=1
    inside = isInsideWoonkern(brievenbus);
    var radius=2.5;
    var lng = brievenbus.geometry.coordinates[0];
    var lat = brievenbus.geometry.coordinates[1];

    if(inside) {
        radius=1;
    }

    id= brievenbus.properties.id;
    var isochroneurl = "https://services.geodan.nl/routing/isochrone?fromcoordx=" + lng +"&fromcoordy=" + lat + "&srs=epsg:4326&networktype=auto&calcmode=distance&calcsize=" + radius + "&steps=1&outputformat=json&servicekey=" + gm_key;
    console.log("curl '" + isochroneurl +"'");
    getIsochrone(isochroneurl,id,function(idret,body,url){
        if(body === undefined){
            console.log("error: " + idret +", " + isochroneurl);
        }
        else{
            // console.log("id: " + idret + " - " + current_record + "/" + max_records);
            var s = JSON.parse(body);
            if(s.features!=null){
                var geom = s.features[0].geometry;
                geom.crs = { type: 'name', properties: { name: 'EPSG:4326'}};
                var sql = "insert into bb_gebieden (id,geom) values (" + idret +", ST_GeomFromGeoJSON('" + JSON.stringify(geom) + "'))"
                var query = client.query(sql);
            }
            else{
                console.log("no features in " + idret + ": "+ body);
            }
        }
        current_record++;
    })
};

console.log('Klaar!');

function getIsochrone(url, id, callback){
    request({url: isochroneurl}, function(err, res, body) {
        if(err){
            console.log(err);
        }
        callback(id,body);

    });
}

function isInsideWoonkern(brievenbus) {
    var isInside = false;
    for(var i=0;i<woonkernen.features.length;i++){
        var woonkern=woonkernen.features[i];
        if(woonkern.properties.aantalinwo>5000){
            if(!isInside){
                isInside = turf.inside(brievenbus,woonkern);
            }
        }
    }
    return isInside;
}