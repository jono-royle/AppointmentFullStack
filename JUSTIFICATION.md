# Architecture Overview
The backend uses a layered architecture with Controller, Service and Repository layers. The React frontend communicates with the controller through HTTP requests, the service layer contains all business logic and the repository handles all data access.
This suits a SaaS product because the separation of concerns allows for better testability, maintainability and scalability - as each layer can be worked on and expanded separately.

# Design Choice
Separating the API and domain models allow each to evolve and change in the future without breaking the other - vital for long term maintainability. With this separation, making changes to the database structure does not break the frontend, and we can easily alter the frontend data provided (or extend it for different clients) without changing the domain. Other separations include ensuring all datetimes stored in the domain are in UTC format, and allowing the frontend to display them in its own timezone.

# Testing Philosophy
Unit tests that validate individual components in isolation are the highest priority because they ensure that the business logic works correctly, and allow for quick bug detection during development and refactoring which keeps the speed of development quick. Integration tests are also valuable as a higher layer of testing between different components, so would be more suited to an established code base rather than something being rapidly developed.

# Trade-offs and Omissions
I did want to focus on the backend logic for this implementation (including adding basic logic for preventing overlapping appointments, which was not explicitly requested but seemed like a natural extension of the business logic). This lead to some trade offs, for example:
Logging - I just used some basic, undetailed console logging. However, I did wrap this in an abstract implementation so that it could be easily swapped out for something more detailed.
Domain model design - I have only designed the bare minimum domain model, as opposed to future proofing with likely to be needed attributes such as the ID of staff assigned to the appointment and an appointment status.
One trade off in particular is the lack of any authentication or authorisation . When designing quickly in an agile team, this is a good option to leave for later as it could slow down early prototyping with complexity, but is not too difficult to add in later.

# Deployment
The service could be containerized using Docker, with a Dockerfile defining the .NET API environment, dependencies, and build steps. The container image would be pushed to Azure Container Registry (ACR) and deployed to Azure App Service. Github action workflow:
1. Trigger on push to main branch
2. Build the docker image and run unit tests, ensure they all pass
3. Tag and push the image to ACR
4. Deploy the container to Azure App service 