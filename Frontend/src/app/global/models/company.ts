import { Product } from './product';
import { ProductMin } from './productMin';
import { TimeSpan } from './timespan';
import { UserMin } from './userMin';

export interface Company {
  Id: number;
  Name: string;
  Description: string;
  OpeningTimeLOC: TimeSpan;
  ClosingTimeLOC: TimeSpan;
  CompanyType: string;
  OwnerId: string;
  OwnerName: string;
  Products: ProductMin[];
  Workers: UserMin[];
  Latitude: number;
  Longitude: number;
}
