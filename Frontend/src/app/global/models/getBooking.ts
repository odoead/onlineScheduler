import { ProductMin } from './productMin';

export interface GetBookings {
  id: number;
  product: ProductMin;
  startDateLOC: Date;
  endDateLOC: Date;
  customerEmail: string;
  customerName: string;
  status: string;
  companyName: string;
}
