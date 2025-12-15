import { AppointmentForm } from "@/features/appointments/AppointmentForm";
import { AppointmentList } from "@/features/appointments/AppointmentList";
import { useAppointments } from "@/features/appointments/useAppointment";

const App = () => {
  const {
    appointments,
    errors,
    successMessage,
    createAppointment,
  } = useAppointments();

  return (
    <div>
      <h1>Appointments</h1>

      <AppointmentForm onSubmit={createAppointment} />

      {successMessage && <p style={{ color: "green" }}>{successMessage}</p>}

      {errors.length > 0 && (
        <ul style={{ color: "red" }}>
          {errors.map((e, i) => (
            <li key={i}>{e}</li>
          ))}
        </ul>
      )}

      <hr />

      <AppointmentList appointments={appointments} />
    </div>
  );
};

export default App;

