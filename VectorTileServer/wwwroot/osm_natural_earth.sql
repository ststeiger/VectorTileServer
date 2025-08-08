
-- SELECT name, geometry,* FROM ne_10m_admin_0_countries WHERE name_en = 'Switzerland';


SELECT name, geometry
,  length(geometry) AS geom_size
-- ,ST_Area(geometry) AS area
-- ,ST_Area(ST_Transform(geometry, 3857)) AS area_m2
,*
FROM ne_10m_admin_1_states_provinces
WHERE (1=1) 
AND admin = 'Switzerland' 
-- AND name_en = 'Kaliningrad' 
ORDER BY geom_size DESC, name
provnum_ne
-- SELECT spatialite_version();
-- SELECT load_extension('mod_spatialite');
