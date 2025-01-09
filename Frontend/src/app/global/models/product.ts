import { CompanyMin } from './companyMin';
import { TimeSpan } from './timespan';
import { UserMin } from './userMin';

export interface Product {
  name: string;
  description: string;
  duration: TimeSpan;
  workers: UserMin[];
  company: CompanyMin;
}
