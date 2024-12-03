import { TimeSpan } from './timespan';

export interface CreateProduct {
  Name: string;
  Description: string;
  Duration: string;
  CompanyId: number;
  WorkerIds: string[];
}
