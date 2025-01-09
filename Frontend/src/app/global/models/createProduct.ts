import { TimeSpan } from './timespan';

export interface CreateProduct {
  name: string;
  description: string;
  duration: string;
  companyId: number;
  workerIds: string[];
}
