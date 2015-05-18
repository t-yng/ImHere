var express = require('express');
var mysql = require('mysql');
var http =require('http');
var app = express();

console.log('server running');

/* Mysqlに接続 */
var connection = mysql.createConnection({
	host: process.env.DB_HOST || 'localhost',
	user: process.env.DB_USER || 'root',
	password: process.env.DB_PASS || 'pass',
	database: process.env.DB_NAME || 'imhere_db'
});

app.get("/", function(req, res){
	res.writeHead(200, {'Content-Type': 'text/plain'});
	res.write('Hello World\n');
	res.end();
});

/* ユーザの新規登録 */
app.get("/imhere/api/add/user/", function(req, res){

	console.log('userAdd');

	/* リクエストパラメータの取得 */
	console.log(req.query);

	/* 特定のパラメータの値を取得 */
	var name = req.query.name;
	var lab = req.query.lab;
	console.log("name ="+name);
	console.log("lab ="+lab);

	register_user(name, lab);

	res.writeHead(200, {'Content-Type': 'text/plain'});
	res.write('Hello World\n');
	res.end();
});

/* 全ユーザの情報と在室状況を返す */
app.get("/imhere/api/get/state/all", function(req, res){

	get_user_all(function(users){
		res.writeHead(200, {'Content-Type': 'text/plain'});
		res.write(JSON.stringify(users));
		res.end();
	});

});

app.get("/imhere/api/change/state/", function(req, res){
	var user_id = req.query.user_id;
	var state = req.query.state;
	change_state(user_id, state, function(result_code){
		if(result_code == 200)
		res.writeHead(200, {'Content-Type': 'text/plain'});
	    res.end();
	});
});

/* port:3000で待機 */
var server = http.createServer(app);
server.listen(3000, function(){
	console.log("server listening on port 3000");
});

/* socket.IOのイスんタンス生成 */
var socketIO = require('socket.io');

/* クライアントの接続を待機（IPアドレスとポート番号を結び付ける） */
var io = socketIO.listen(server);

/* クライアントが接続してきたときの処理 */
io.sockets.on('connection', function(socket){
	console.log("connection");

	/* メンバーの状態変更通知 */
	socket.on('c2s_update_state', function(data){
		console.log("[update_state] recieved message : "+JSON.stringify(data));

		/* 接続している全クライアントに送信 */
		io.sockets.emit('s2c_update_state', {user_id: data.user_id, state : data.state});

		/* 送信クライアント以外の全クライアントに送信（ブロードキャスト） */
		// socket.broadcat.emit('s2c_message', {value : data.value});
	});

	/* メンバー登録リクエスト受付 */
	socket.on('c2s_message_register_member',function(data){
		console.log("[register_member] recieved message :"+JSON.stringify(data));

		var name = data.name;
		var lab = data.lab;

		register_user(name, lab, function(user_id){
			get_user(user_id, function(user){
				io.sockets.emit('s2c_message_register_member', user);
			});
		});


	});

	/* クライアントが切断したときの処理 */
	socket.on('disconnect', function(){
		console.log('disconnect');
	});
})

/* DBへ新規ユーザを登録 */
function register_user(name, lab, callback){
	/* userテーブルにユーザを追加 */
	var sql = "insert into user(name, lab) values(?, ?)";

	connection.query(sql, [name,lab], function(err, info){
		if(err){
			throw err;	
		} 
		else{
			var user_id = info.insertId;

			/* user_stateテーブルにユーザを追加 */
			sql = "insert into user_state(user_id, state) values(?, 0)";
			connection.query(sql, [user_id], function(err, info){
				if(err) throw err;

				callback(user_id);
			});

		}
	});
}

/* 特定ユーザーの在室状況を変更 */
function change_state(user_id, state, callback){
	/* 在室状況の更新 */
	var sql = "update user_state set state=? where user_id=?";

	connection.query(sql,[state, user_id], function(err, result){
		if(!err){
			console.log("update state : user_id="+user_id+", state="+state);
			callback(200);
		}
	});
}

/* 特定のユーザ情報をDBより取得 */
function get_user(user_id,  callback){
	var sql =  "select id, name, lab, state from user left join user_state on user.id = user_state.user_id where user_id = ?";
	connection.query(sql, [user_id], function(err, result){
		if(err){
			console.log(err);
		}
		else{
			// console.log(result);
			callback(result[0]);
		}
	});
}

/* 全ユーザの情報と在室状況をDBより取得 */
function get_user_all(callback){
	var sql = "select id from user";

	var response_array = [];
	var state_array = [];

	connection.query(sql, function(err, result){
		for(var i in result){

			/* 各ユーザのIDを取得 */
			var user_id = result[i]['id'];

			/* 各ユーザの情報を配列に格納 */
			get_user(user_id, function(user){
				response_array.push(user);

				console.log('response='+response_array.length+", result = "+result.length);

				/* 全てユーザの情報の取得が完了したらJSONとして返す */
				if(response_array.length == result.length){
					callback(response_array);
				}
			});
		}
	});
}
