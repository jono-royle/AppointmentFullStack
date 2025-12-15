import type { Appointment } from "./appointment.types";

interface Props {
  appointments: Appointment[];
}

export const AppointmentList = ({ appointments }: Props) => {
  if (appointments.length === 0) {
    return <p>No appointments yet</p>;
  }

  return (
    <ul>
      {appointments.map((a) => (
        <li key={a.id}>
          <strong>{a.clientName}</strong> –{" "}
          {new Date(a.appointmentTime).toLocaleString()} (
          {a.serviceDurationMinutes} mins)
        </li>
      ))}
    </ul>
  );
};
