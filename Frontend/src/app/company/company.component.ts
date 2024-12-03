import { Component } from '@angular/core';
import { Company } from '../global/models/company';
import { CompanyService } from '../global/services/company.service';
import { MatDialog } from '@angular/material/dialog';
import { Router } from 'express';
import { ProductService } from '../global/services/product.service';
import { MatCard, MatCardActions, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle } from '@angular/material/card';
import { MatList, MatListItem } from '@angular/material/list';
import { MatIcon } from '@angular/material/icon';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-company',
  standalone: true,
  imports: [MatCard,MatCardContent,MatCardHeader,MatCardTitle,MatCardSubtitle,MatList,MatListItem,MatIcon,MatCardActions],
  templateUrl: './company.component.html',
  styleUrl: './company.component.css'
})
export class CompanyComponent {
  company!: Company;

  constructor(private companyService:CompanyService,private dialog: MatDialog, private router:Router, private productService:ProductService,   private route: ActivatedRoute,  ) {
    
  }
  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const companyId = +!params.get('id');
      this.getCompany(companyId,true);
    });
  }

  getCompany(id:number,isPageLoad:boolean):void{
    this.companyService.getCompany(id,isPageLoad).subscribe({
     next :(result) =>  {
      this.company= result;
     }
    });
  }
}
