CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(100) NOT NULL,
    Lastname VARCHAR(100) NOT NULL,
    Birthday DATETIME NOT NULL
);



CREATE TABLE IF NOT EXIST block (

    height int(11) NOT NULL,
  
    dateblock datetime NOT NULL,
  
    hash longtext NOT NULL,
  
    sizeblock varchar(10) NOT NULL,
  
    shared varchar(12) NOT NULL,
  
    resolvedby varchar(1000) NOT NULL,
  
    difficulty varchar(20) NOT NULL,
  
    rewardblock varchar(20) NOT NULL,
  
    status int(1) NOT NULL,
  
    raizmerkle longtext NOT NULL

);