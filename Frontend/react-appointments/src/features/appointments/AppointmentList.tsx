import type { Appointment } from "./appointment.types";

interface Props {
  appointments: Appointment[];
}

export const AppointmentList = ({ appointments }: Props) => {
  if (appointments.length === 0)
    return (
      <p className="text-center text-gray-500 dark:text-gray-400 mt-4">
        No appointments yet.
      </p>
    );

  return (
    <div className="max-w-md mx-auto mt-6 space-y-4">
      {appointments.map((appt) => (
        <div
          key={appt.id}
          className="p-4 bg-white dark:bg-gray-800 rounded-md shadow flex flex-col space-y-1"
        >
          <p className="text-gray-700 dark:text-gray-200">
            <span className="font-semibold">Client:</span> {appt.clientName}
          </p>
          <p className="text-gray-700 dark:text-gray-200">
            <span className="font-semibold">Time:</span> 
            {new Date(appt.appointmentTime).toLocaleDateString()}{", "}
            {new Date(appt.appointmentTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
          </p>
          {appt.serviceDurationMinutes !== undefined && (
            <p className="text-gray-700 dark:text-gray-200">
              <span className="font-semibold">Duration:</span> {appt.serviceDurationMinutes} min
            </p>
          )}
        </div>
      ))}
    </div>
  );
};
