import { HttpHeaders } from "@angular/common/http";

export class HeadersService {
    public static getPageLoad(): HttpHeaders {
      return new HttpHeaders().set('Page-Load', 'true');
    }
  }