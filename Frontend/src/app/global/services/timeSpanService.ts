import { TimeSpan } from '../models/timespan';

export class TimeSpanService {
  public formatTimeSpan(time: TimeSpan): string {
    if (!time) return '';
    const hours = time.hr.toString().padStart(2, '0');
    const minutes = time.min.toString().padStart(2, '0');
    return `${hours}:${minutes}:00`;
  }

  public formatTimePicker(time: any): string {
    return `${time}:00`;
  }

  public getTimeSpanMinuteDuration(time: TimeSpan): number {
    const totalMin = time.hr * 60 + time.min;
    return totalMin;
  }

  public minutesToTimeSpan(minutes: number): TimeSpan {
    const hr = Math.floor(minutes / 60);
    const min = Math.floor(minutes % 60);
    const sec = 0;
    return { hr, min, sec };
  }
}
