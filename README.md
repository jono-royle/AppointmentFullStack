# AppointmentFullStack

Submission for Senior Full-Stack Engineer Case Study for Jonathan Royle

Setup and run instructions:

Backend - Navigate to AppointmentFullStack\API\AppointmentAPI and use command 'dotnet run'
The backend will now be listening on a local HTTP or HTTPS port shown in the console output, and the swagger can be viewed on localhost:{port}/swagger

Frontend - Navigate to AppointmentFullStack\Frontend\react-appointments. Create or update the file '.env.local' in this directory and set the backend URL in that file as follows:
VITE_BACKEND_URL=http://localhost:{port}
or
VITE_BACKEND_URL=https://localhost:{port}
Then, run the frontend react site with command 'npm run dev' and open the URL shown in the console

