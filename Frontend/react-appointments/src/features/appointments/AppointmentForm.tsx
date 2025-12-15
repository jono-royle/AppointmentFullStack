import { useState } from "react";
import type { FormEvent } from "react";
import type { IngestAppointmentRequest } from "./appointment.types";

interface Props {
  onSubmit: (request: IngestAppointmentRequest) => void;
}

export const AppointmentForm = ({ onSubmit }: Props) => {
  const [clientName, setClientName] = useState("");
  const [appointmentTime, setAppointmentTime] = useState("");
  const [serviceDurationMinutes, setServiceDurationMinutes] = useState<number | undefined>(undefined);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();

    onSubmit({
      clientName,
      appointmentTime,
      serviceDurationMinutes,
    });
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Client Name</label>
        <input
          value={clientName}
          onChange={(e) => setClientName(e.target.value)}
          required
        />
      </div>

      <div>
        <label>Appointment Time</label>
        <input
          type="datetime-local"
          value={appointmentTime}
          onChange={(e) => setAppointmentTime(e.target.value)}
          required
        />
      </div>

      <div>
        <label>Service Duration (minutes)</label>
        <input
        type="number"
        value={serviceDurationMinutes ?? ""}
        onChange={(e) => {
            const value = e.target.value;
            setServiceDurationMinutes(value === "" ? undefined : Number(value));
  }}
/>
      </div>

      <button type="submit">Create Appointment</button>
    </form>
  );
};
