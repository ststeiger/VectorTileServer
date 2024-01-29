
-- Yes, it is possible. This feature was added on 2020-01-22 - SQLite 3.31.0
-- https://www.sqlite.org/gencol.html

CREATE TABLE t1
(
	 a INTEGER PRIMARY KEY
	,b INT
	,c TEXT
	,d INT GENERATED ALWAYS AS ( a*abs(b) ) VIRTUAL
	,e TEXT GENERATED ALWAYS AS ( substr(c, b, b+1) ) STORED
); 
