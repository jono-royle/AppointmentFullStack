import { useState } from "react";
import type { FormEvent } from "react";
import type { IngestAppointmentRequest } from "./appointment.types";

interface Props {
  onSubmit: (request: IngestAppointmentRequest) => void;
}

export const AppointmentForm = ({ onSubmit }: Props) => {
  const [clientName, setClientName] = useState("");
  const [appointmentTime, setAppointmentTime] = useState("");
  const [serviceDurationMinutes, setServiceDurationMinutes] =
    useState<number | undefined>(undefined);

  const [errors, setErrors] = useState<string[]>([]);

const validate = (): boolean => {
  const errs: string[] = [];

  // Client name validation
  if (clientName.trim().length < 1) {
    errs.push("Client name must be at least 1 character.");
  }
  if (clientName.trim().length > 100) {
    errs.push("Client name cannot exceed 100 characters.");
  }

  // Appointment time validation
  if (appointmentTime) {
    const date = new Date(appointmentTime);
    const now = new Date();

    const minutes = date.getMinutes();
    if (minutes !== 0 && minutes !== 30) {
      errs.push("Appointment time must be on the hour or half hour.");
    }

    // At least 5 minutes in the future
    const minDate = new Date(now.getTime() + 5 * 60 * 1000);
    if (date < minDate) {
      errs.push("Appointment time must be at least 5 minutes in the future.");
    }
  } else {
    errs.push("Appointment time is required.");
  }

  setErrors(errs);
  return errs.length === 0;
};


  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    onSubmit({
      clientName: clientName.trim(),
      appointmentTime: new Date(appointmentTime).toISOString(),
      serviceDurationMinutes,
    });

    // Reset form
    setClientName("");
    setAppointmentTime("");
    setServiceDurationMinutes(undefined);
    setErrors([]);
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="max-w-md mx-auto my-8 p-6 bg-white dark:bg-gray-800 rounded-lg shadow-md space-y-4"
    >
      <h2 className="text-xl font-semibold text-gray-700 dark:text-gray-200">
        Create Appointment
      </h2>

      {errors.length > 0 && (
        <div className="text-red-600 dark:text-red-400 space-y-1">
          {errors.map((err, idx) => (
            <p key={idx}>{err}</p>
          ))}
        </div>
      )}

      <div className="flex flex-col">
        <label className="mb-1 text-gray-600 dark:text-gray-300">Client Name</label>
        <input
          type="text"
          value={clientName}
          onChange={(e) => setClientName(e.target.value)}
          required
          className="border border-gray-300 dark:border-gray-600 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 dark:bg-gray-700 dark:text-gray-200"
        />
      </div>

      <div className="flex flex-col">
        <label className="mb-1 text-gray-600 dark:text-gray-300">Appointment Time</label>
        <input
          type="datetime-local"
          value={appointmentTime}
          onChange={(e) => setAppointmentTime(e.target.value)}
          required
          className="border border-gray-300 dark:border-gray-600 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 dark:bg-gray-700 dark:text-gray-200"
        />
      </div>

      <div className="flex flex-col">
        <label className="mb-1 text-gray-600 dark:text-gray-300">
          Service Duration (minutes)
        </label>
        <input
          type="number"
          value={serviceDurationMinutes ?? ""}
          onChange={(e) => {
            const value = e.target.value;
            setServiceDurationMinutes(value === "" ? undefined : Number(value));
          }}
          className="border border-gray-300 dark:border-gray-600 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 dark:bg-gray-700 dark:text-gray-200"
        />
      </div>

      <button
        type="submit"
        className="w-full bg-blue-500 hover:bg-blue-600 text-white py-2 rounded-md transition"
      >
        Create Appointment
      </button>
    </form>
  );
};
