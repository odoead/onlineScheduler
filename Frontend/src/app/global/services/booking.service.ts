import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { CreateBooking } from '../models/createBooking';
import { Observable } from 'rxjs';
import { HeadersService } from './headerService';
import { GetBookings } from '../models/getBooking';
import { BookingStatus } from '../models/bookingStatus';
@Injectable({
  providedIn: 'root'
})
export class BookingService {

  baseUrl= environment.bookingUrl;
  constructor(private http: HttpClient, ) { }

  public addBooking(bookingData:CreateBooking): Observable<number> {
    return this.http.post<number>(`${this.baseUrl}/api/booking/`, bookingData);
  }

  public getBookings(isPageLoad:boolean):Observable<GetBookings[]>
  {   
     const headers = isPageLoad ? HeadersService.getPageLoad() : undefined;
    return this.http.get<GetBookings[]>(`${environment.companyUrl}/api/booking`,{headers})
  }
  
  /*
  public editBooking(bookingId: number, workerId:string,BookingTimeLOC:Date): Observable<void> {
    const body = {
      UserEmails: userEmails,
    };
    const headers =  HeadersService.getPageLoad();
    return this.http.put<void>(`${this.baseUrl}/api/company/${companyId}/employees`, body,{headers});
  }*/

  changeBookingStatus(id: number, newStatus: BookingStatus):Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/api/booking/status/${id}`, { newStatus });
  }

}
