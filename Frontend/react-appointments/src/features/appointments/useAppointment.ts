import { useState } from "react";
import type { Appointment, IngestAppointmentRequest } from "./appointment.types";
import {
  ingestAppointment,
  getAppointmentById,
} from "./appointment.api";

export const useAppointments = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [errors, setErrors] = useState<string[]>([]);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

const createAppointment = async (
  request: IngestAppointmentRequest
) => {
  setErrors([]);
  setSuccessMessage(null);

  try {
    const { id, message } = await ingestAppointment(request);

    const appointment = await getAppointmentById(id);

    setAppointments((prev) => [...prev, appointment]);
    setSuccessMessage(message);
  } catch (err: unknown) {
    console.error("Appointment error:", err);

    if (
      typeof err === "object" &&
      err !== null &&
      "errors" in err &&
      Array.isArray((err as any).errors)
    ) {
      setErrors((err as any).errors);
    } else {
      setErrors(["An unexpected error occurred"]);
    }
  }
};


  return {
    appointments,
    errors,
    successMessage,
    createAppointment,
  };
};
