
namespace VectorTileSelector
{


    internal class SizeUpdater
    {

        public static async System.Threading.Tasks.Task UpdateSizeAsync()
        {
            string sql = @"
;WITH CTE AS 
( 
	SELECT 
		 continent 
		,subregion 
		,CASE 
			WHEN LOWER(continent) = 'earth' THEN LOWER(region_name) + '.kml' 
			WHEN LOWER(continent) = 'brazil' THEN 'south-america/' + LOWER(continent) + '/' + LOWER(region_name) + '.kml' 
			WHEN LOWER(continent) IN ('canada', 'us' ) THEN 'north-america/' + LOWER(continent) + '/' + LOWER(region_name) + '.kml' 
			WHEN LOWER(region_name) = 'russia' THEN LOWER(region_name) + '.kml' 
			ELSE continent + '/' + LOWER(region_name) + '.kml' 
		END AS download_name 
	FROM region_data 
    WHERE spheric_area_m2 IS NULL 
    OR equal_area_world_area_m2 IS NULL 
) 
SELECT 
	 continent 
	,subregion 
	,REPLACE(download_name, '/', '-') AS ""file_name"" 
FROM CTE
";

            string kmlDirectory = Db.KmlDirectory;

            string sqlUpdate = @"
UPDATE region_data 
    SET  spheric_area_m2 = @SphericArea 
        ,equal_area_world_area_m2 = @CylindricalEqualAreaworld 
WHERE continent = @Continent 
AND subregion = @Subregion 
";
            using (System.Data.Common.DbConnection updateConnection = Db.Connection)
            {

                if (updateConnection.State != System.Data.ConnectionState.Open)
                    await updateConnection.OpenAsync();

                using (System.Data.Common.DbConnection readConnection = Db.Connection)
                {

                    using (System.Data.Common.DbCommand readCommand = readConnection.CreateCommand())
                    {
                        readCommand.CommandText = sql;

                        if (readConnection.State != System.Data.ConnectionState.Open)
                            await readConnection.OpenAsync();

                        using (System.Data.Common.DbDataReader reader = await readCommand.ExecuteReaderAsync())
                        {
                            int CONTINENT = reader.GetOrdinal("continent");
                            int SUBREGION = reader.GetOrdinal("subregion");
                            int FILE_NAME = reader.GetOrdinal("file_name");

                            while (await reader.ReadAsync())
                            {
                                string continent = reader.GetString(CONTINENT);
                                string subregion = reader.GetString(SUBREGION);
                                string file_name = reader.GetString(FILE_NAME);


                                string boundaryFile = System.IO.Path.Combine(kmlDirectory, file_name);
                                Xml2CSharp.KmlRegionBoundaryXml region_boundaries = Xml2CSharp.KmlRegionBoundaryXml.DeserializeFile(boundaryFile);

                                await System.Console.Out.WriteLineAsync(region_boundaries.Document.Placemark.MultiGeometry.Polygon.OuterBoundaryIs.LinearRing.CoordinateList.ToString());

                                Xml2CSharp.RectBounds bounds = region_boundaries.Document.Placemark.MultiGeometry.Polygon.OuterBoundaryIs.LinearRing.RectangularBounds;
                                double area = region_boundaries.Document.Placemark.MultiGeometry.Polygon.OuterBoundaryIs.LinearRing.SphericalPolygonArea;
                                double mollweide_area = MollweideArea.CalculatePolygonArea(region_boundaries.Document.Placemark.MultiGeometry.Polygon.OuterBoundaryIs.LinearRing.CoordinateList);


                                string area_meters = area.ToString("N0");
                                // Burundi spheric: 28'034'768'830
                                // Burundi actual:  27'834'000'000 = 27,834 km² 

                                System.Console.WriteLine(area_meters);
                                System.Console.WriteLine(bounds);

                                await Dapper.SqlMapper.ExecuteAsync(updateConnection, sqlUpdate, new
                                {
                                    SphericArea = (long)area, // Replace with the actual value
                                    CylindricalEqualAreaworld = (long)mollweide_area, // Replace with the actual value
                                    Continent = continent, // Replace with the actual value
                                    Subregion = subregion // Replace with the actual value
                                });

                            } // Next 

                        } // End Using reader 

                    } // End using command 

                    if (readConnection.State != System.Data.ConnectionState.Closed)
                        await readConnection.CloseAsync();
                } // End Using readConnection 


                if (updateConnection.State != System.Data.ConnectionState.Closed)
                    await updateConnection.CloseAsync();
            } // End Using updateConnection 

        } // End Task FetchAndDownloadAsync 


    }
}
