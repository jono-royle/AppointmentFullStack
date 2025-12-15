export interface Appointment {
  id: number;
  clientName: string;
  appointmentTime: string; // ISO string
  serviceDurationMinutes: number;
}

export interface IngestAppointmentRequest {
  clientName: string;
  appointmentTime: string;
  serviceDurationMinutes?: number;
}

export interface IngestAppointmentResponse {
  appointmentId: string;
  message: string;
}

export interface ApiErrorResponse {
  errors: string[];
}
