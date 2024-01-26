
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
				WHEN size >= 1024^4 THEN (size / (1024^4))::DECIMAL(18, 2) 
				WHEN size >= 1024^3 THEN (size / (1024^3))::DECIMAL(18, 2) 
				WHEN size >= 1024^2 THEN (size / (1024^2))::DECIMAL(18, 2) 
				WHEN size >= 1024 THEN (size / 1024)::DECIMAL(18, 2) 
				ELSE size::DECIMAL(18, 2) 
			END 
		AS character varying(30)) 
		|| ' B'
    ) STORED
	
	,region_name_with_prefix character varying(255) GENERATED ALWAYS AS 
	(
        LEFT(href, LENGTH(href) - LENGTH('-latest.osm.pbf'))
    ) STORED
	
);
