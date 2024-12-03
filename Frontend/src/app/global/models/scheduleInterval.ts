import { Booking } from "./booking"
import { TimeSpan } from "./timespan"

 export interface ScheduleInterval{
      WeekDay :number
      StartTimeLOC:TimeSpan 
      FinishTimeLOC :TimeSpan
      Bookings :Booking[]
      IntervalType:string;
}