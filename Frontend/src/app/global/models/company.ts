import { Product } from './product';
import { ProductMin } from './productMin';
import { TimeSpan } from './timespan';
import { UserMin } from './userMin';

export interface Company {
  id: number;
  name: string;
  description: string;
  openingTimeLOC: TimeSpan;
  closingTimeLOC: TimeSpan;
  companyType: string;
  ownerId: string;
  ownerName: string;
  products: ProductMin[];
  workers: UserMin[];
  latitude: number;
  longitude: number;
}
