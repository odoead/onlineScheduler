import { CompanyMin } from './companyMin';
import { TimeSpan } from './timespan';
import { UserMin } from './userMin';

export interface Product {
  Name: string;
  Description: string;
  Duration: TimeSpan;
  Workers: UserMin[];
  Company: CompanyMin;
}
