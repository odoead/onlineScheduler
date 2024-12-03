import { ProductMin } from './productMin';

export interface GetBookings {
  Id: number;
  Product: ProductMin;
  StartDateLOC: Date;
  EndDateLOC: Date;
  CustomerEmail: string;
  CustomerName: string;
  Status: string;
  CompanyName: string;
}
