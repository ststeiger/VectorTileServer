
SELECT 
	 continent
	,subregion
	 
	,size
	,spheric_area_m2
	,equal_area_world_area_m2
	,tile_size
	 
	,'UPDATE region_data 
	  SET  size = ' + CAST(size AS varchar(50)) + ' 
          ,spheric_area_m2 = + ' + CAST(spheric_area_m2 AS varchar(50)) + ' 
		  ,equal_area_world_area_m2 = ' + CAST(equal_area_world_area_m2 AS varchar(50)) + ' 
		  ,tile_size = ' + CAST(tile_size AS varchar(50)) + ' 
WHERE continent = ''' + continent + ''' 
AND subregion = ''' + subregion + ''' 
; ' AS sql 

FROM region_data 
