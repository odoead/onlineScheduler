import { TimeSpan } from './timespan';

export interface UpdateProduct {
  name: string;
  description: string;
  duration: string;
  workerIds: string[];
}
