HALL RENT - POSTMAN ONE CLICK

1. Start the API in Development mode.
   URL: http://localhost:5226
   Swagger: http://localhost:5226/swagger

2. Import both files into Postman:
   - Hall_rent_Postman_All_API.postman_collection.json
   - Hall_rent_Postman_Local.postman_environment.json

3. Select environment: Hall Rent - Local.

4. Open the collection and click Run.
   The collection is ordered and chained automatically.
   It creates a unique Favor, creates a Hall using that Favor, searches the Hall,
   updates both entities, creates a booking, checks overlapping-booking rejection,
   checks revenue analytics, checks top-favors analytics, then deletes the Hall.

5. Expected result: all requests in the run should be green.

If HTTPS redirect causes a certificate warning, either disable SSL certificate verification
in Postman or change the environment baseUrl to https://localhost:7000.
