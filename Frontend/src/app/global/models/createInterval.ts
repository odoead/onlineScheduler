import { TimeSpan } from "./timespan";

export interface CreateInterval {
  WeekDay: number;
  StartTimeLOC: TimeSpan;
  FinishTimeLOC: TimeSpan;
  IntervalType: number;
  EmployeeId: string;
  CompanyId: number;
}
