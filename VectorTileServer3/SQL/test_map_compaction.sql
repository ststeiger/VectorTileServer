
-- DROP TABLE YourTable;

CREATE TABLE YourTable 
(
	 tms INTEGER 
	,tms_tile_data blob 
	,CONSTRAINT tmsXY_unique UNIQUE (tms) 
);


CREATE UNIQUE INDEX uix_tmsXY ON YourTable (tms);
COMMIT;




INSERT INTO YourTable (tmsX, tmsY) VALUES (100, 200);
INSERT INTO YourTable (tmsX, tmsY) VALUES (500, 700);
INSERT INTO YourTable (tmsX, tmsY) VALUES (123, 456);
INSERT INTO YourTable (tmsX, tmsY) VALUES (0, 123);
INSERT INTO YourTable (tmsX, tmsY) VALUES (1, 123);

UPDATE YourTable SET tms = (CAST(tmsX AS INTEGER) << 32) | CAST(tmsY AS INTEGER);



INSERT INTO YourTable (tmsX, tmsY) VALUES (100, 200);
INSERT INTO YourTable (tmsX, tmsY) VALUES (500, 700);
INSERT INTO YourTable (tmsX, tmsY) VALUES (123, 456);
INSERT INTO YourTable (tmsX, tmsY) VALUES (0, 123);
INSERT INTO YourTable (tmsX, tmsY) VALUES (1, 123);





SELECT * 
FROM YourTable 
-- WHERE tms =  ((1<<32 )+123)
