
-- DROP TABLE tms_map

CREATE TABLE tms_map 
(
	 tms_zoom INTEGER 
	,tms_xy INTEGER 
	,tms_tile blob 
	,CONSTRAINT tmsXY_unique UNIQUE (tms_zoom, tms_xy) 
);


CREATE UNIQUE INDEX uix_tmsXY ON tms_map (tms_zoom, tms_xy);
COMMIT;

INSERT INTO tms_map (tms_zoom, tms_xy, tms_tile)  
SELECT 
	 map.zoom_level AS tms_zoom 
	,(map.tile_column << 32) | map.tile_row AS tms_xy 
	,images.tile_data AS tms_tile 
FROM map
INNER JOIN images ON images.tile_id = map.tile_id
-- WHERE map.zoom_level = 1
;


SELECT * 
FROM tms_map 
WHERE (1=1) 
AND tms_zoom = 14
AND tms_xy = 36631776077993 -- ((1<<32 )+123)



SELECT * 
FROM tms_map 
-- WHERE tms_xy =  ((1<<32 )+123)
