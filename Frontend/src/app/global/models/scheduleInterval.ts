import { Booking } from "./booking"
import { TimeSpan } from "./timespan"

 export interface ScheduleInterval{
      weekDay :number
      startTimeLOC:TimeSpan 
      finishTimeLOC :TimeSpan
      bookings :Booking[]
      intervalType:string;
}