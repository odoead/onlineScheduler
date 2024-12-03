import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Company } from '../models/company';
import { CreateCompany } from '../models/createCompany';
import { Product } from '../models/product';
import { CreateProduct } from '../models/createProduct';
import { UpdateProduct } from '../models/updateProduct';
import { HeadersService } from './headerService';

@Injectable({
  providedIn: 'root'
})
export class ProductService { 
    baseUrl= environment.companyUrl;
    constructor(private http: HttpClient) { }

    addProduct(product: CreateProduct): Observable<number> {
      const headers =  HeadersService.getPageLoad();
      return this.http.post<number>(`${this.baseUrl}/api/product/`, product, { headers });
    }
  
    getProduct(id: number, isPageLoad: boolean): Observable<Product> {
      const headers = isPageLoad ? HeadersService.getPageLoad() : undefined;
      return this.http.get<Product>(`${this.baseUrl}/api/product/${id}`,  {headers} );
    }
  
    updateProduct(id: number, updateProductDTO: UpdateProduct): Observable<void> {
      const headers =  HeadersService.getPageLoad();
      return this.http.put<void>(`${this.baseUrl}/api/product/${id}`, updateProductDTO,{headers});
    }
  
    deleteProduct(id: number): Observable<void> {
      const headers =  HeadersService.getPageLoad();
      return this.http.delete<void>(`${this.baseUrl}/api/product/${id}`,{headers});
    }
   
}