
-- Drop the table if it exists
DROP TABLE IF EXISTS region_data;


-- Create the table region_data
CREATE TABLE region_data 
( 
     continent character varying(255) 
	,subregion character varying(255) 
	,href character varying(255) 
	,size bigint 
	,is_special_subregion boolean 
	,spheric_area_m2 bigint 
	,equal_area_world_area_m2 bigint 
	,tile_size bigint 
	 
	 
	,region_name character varying(255) GENERATED ALWAYS AS 
	(
        CASE
            WHEN href LIKE '%-latest.osm.pbf' THEN SUBSTRING(href, POSITION('/' IN href) + 1, LENGTH(href) - POSITION('/' IN href) - LENGTH('-latest.osm.pbf'))
            ELSE SUBSTRING(href, POSITION('/' IN href) + 1)
        END
    ) STORED

	,size_for_humans character varying(50) GENERATED ALWAYS AS 
    (
		CAST
		(
			CASE
				WHEN size >= 1024^4 THEN CAST((size / (1024^4))::DECIMAL(18, 2) AS character varying(30)) || 'TB' 
				WHEN size >= 1024^3 THEN CAST((size / (1024^3))::DECIMAL(18, 2) AS character varying(30)) || 'GB' 
				WHEN size >= 1024^2 THEN CAST((size / (1024^2))::DECIMAL(18, 2) AS character varying(30)) || 'MB' 
				WHEN size >= 1024 THEN CAST((size / 1024)::DECIMAL(18, 2) AS character varying(30)) || 'KB' 
				ELSE CAST(size::DECIMAL(18, 2) AS character varying(30)) || 'B' 
			END 
			AS character varying(30)
		)
    ) STORED
	 
	,tile_size_for_humans character varying(50) GENERATED ALWAYS AS 
    (
		CAST
		(
			CASE
				WHEN tile_size >= 1024^4 THEN CAST((tile_size / (1024^4))::DECIMAL(18, 2) AS character varying(30)) || 'TB' 
				WHEN tile_size >= 1024^3 THEN CAST((tile_size / (1024^3))::DECIMAL(18, 2) AS character varying(30)) || 'GB' 
				WHEN tile_size >= 1024^2 THEN CAST((tile_size / (1024^2))::DECIMAL(18, 2) AS character varying(30)) || 'MB' 
				WHEN tile_size >= 1024 THEN CAST((tile_size / 1024)::DECIMAL(18, 2) AS character varying(30)) || 'KB' 
				ELSE CAST(tile_size::DECIMAL(18, 2) AS character varying(30)) || 'B' 
			END 
			AS character varying(30)
		)
     ) STORED
	 
	,region_name_with_prefix character varying(255) GENERATED ALWAYS AS 
	(
        LEFT(href, LENGTH(href) - LENGTH('-latest.osm.pbf'))
    ) STORED
	
);
