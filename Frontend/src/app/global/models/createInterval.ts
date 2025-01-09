import { TimeSpan } from "./timespan";

export interface CreateInterval {
  weekDay: number;
  startTimeLOC: TimeSpan;
  finishTimeLOC: TimeSpan;
  intervalType: number;
  employeeId: string;
  companyId: number;
}
