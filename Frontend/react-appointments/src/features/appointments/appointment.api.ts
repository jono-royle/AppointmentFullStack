import type {
  Appointment,
  IngestAppointmentRequest,
  IngestAppointmentResponse,
} from "./appointment.types";
import { httpGet } from "@/api/httpClient";

export async function ingestAppointment(
  request: IngestAppointmentRequest
): Promise<{ id: string; message: string }> {
  const response = await fetch("/api/appointment/ingest", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  const data: IngestAppointmentResponse = await response.json();

  if (!response.ok) {
    throw data;
  }

  // Normalize backend response here
  return {
    id: data.appointmentId,
    message: data.message,
  };
}

export function getAppointmentById(id: string): Promise<Appointment> {
  return httpGet<Appointment>(`/appointment/${id}`);
}
