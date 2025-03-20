import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Company } from '../models/company';
import { CreateCompany } from '../models/createCompany';
import { HeadersService } from './headerService';
import { CompanyMin } from '../models/companyMin';

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
 baseUrl= environment.companyUrl;
  constructor() { }
  private http = inject(HttpClient);
  public getCompany(Id: number,isPageLoad:boolean): Observable<Company> {
    const headers = isPageLoad ? HeadersService.getPageLoad() : undefined;
    return this.http.get<Company>(`${this.baseUrl}/api/company/${Id}`,{headers});
  }

  public deleteCompany(Id: number): Observable<boolean> {
    const headers =  HeadersService.getPageLoad();
    return this.http.delete<boolean>(`${this.baseUrl}/api/company/${Id}`,{headers});
  }

  public addEmployeesToCompany(companyId: number, userEmails: string[]): Observable<void> {
    const body = {
      UserEmails: userEmails,
    };
    const headers =  HeadersService.getPageLoad();
    return this.http.post<void>(`${this.baseUrl}/api/company/${companyId}/employees`, body,{headers});
  }

  public removeEmployeeFromCompany(companyId: number, workerId: string): Observable<boolean> {
    const headers =  HeadersService.getPageLoad();
    return this.http.delete<boolean>(`${this.baseUrl}/api/company/${companyId}/employees/${workerId}`,{headers});
  }

  public addCompany(createCompany:CreateCompany):Observable<number> {
    const headers =  HeadersService.getPageLoad();
    return this.http.post<number>(`${this.baseUrl}/api/company`, createCompany,{headers});
  }

  public getCompaniesMin():Observable<CompanyMin[]>{
    const headers =  HeadersService.getPageLoad();
    return this.http.get<CompanyMin[]>(`${this.baseUrl}/api/company` ,{headers});
  }

}
