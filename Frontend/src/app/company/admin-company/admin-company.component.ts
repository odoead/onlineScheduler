import { Component, OnInit } from '@angular/core';
import { CompanyService } from '../../global/services/company.service';
import { Company } from '../../global/models/company';
import {MatDialog } from  '@angular/material/dialog'
import { ActivatedRoute, Router } from '@angular/router';
import {MatCard , MatCardActions, MatCardContent, MatCardHeader, MatCardSubtitle, MatCardTitle, } from '@angular/material/card'
import{MatList, MatListItem} from '@angular/material/list'
import {MatIcon} from '@angular/material/icon'
import { AddEmployeeComponent } from '../add-employee/add-employee.component';
import { ConfirmDialogComponent } from '../../global/confirm-dialog/confirm-dialog.component';
import { ProductService } from '../../global/services/product.service';
import { EditProductComponent } from '../../product/edit-product/edit-product.component';
import { AddProductComponent } from '../add-product/add-product.component';
import { AuthService } from '../../global/services/auth.service';
@Component({
  selector: 'app-admin-company',
  standalone: true,
  imports: [MatCard,MatCardContent,MatCardHeader,MatCardTitle,MatCardSubtitle,MatList,MatListItem,MatIcon,MatCardActions],
  templateUrl: './admin-company.component.html',
  styleUrl: './admin-company.component.css'
})
export class AdminCompanyComponent implements OnInit {
  company!: Company;

  constructor(private companyService:CompanyService,private dialog: MatDialog, private router:Router, private productService:ProductService,
    private route: ActivatedRoute, private authService:AuthService ) {
    
  }
  ngOnInit(): void {
    this.authService.getOwnerCompanyId().subscribe((param) => {
      if(param!== null)
      {
        this.getCompany(param,true);
      }
      else{
        this.router.navigate(["/forbidden"]);
      }
    });
  }

  getCompany(id:number,isPageLoad:boolean):void{
    this.companyService.getCompany(id,isPageLoad).subscribe({
     next :(result) =>  {
      this.company= result;
     }
    });
  }

  addEmployee(): void {
    const dialogRef = this.dialog.open(AddEmployeeComponent, {
      width: '400px',
      data: { companyId: this.company.Id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.getCompany(this.company.Id,false);
      }
    });
  }

  removeEmployee(workerId: string): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: { 
        title: 'Remove Employee', 
        message: 'Are you sure you want to remove this employee from the company?' 
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.companyService.removeEmployeeFromCompany(this.company.Id, workerId)
          .subscribe({
            next: (result) => {
              alert('Employee removed successfully');
              // Refresh company details
              this.getCompany(this.company.Id, true);
            },
            error: () => {
              alert('Failed to remove employee');
            }
          });
      }
    });
  }

  deleteCompany(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: { 
        title: 'Delete Company', 
        message: 'Are you sure you want to delete this company? This action cannot be undone.' 
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.companyService.deleteCompany(this.company.Id)
          .subscribe({
            next: () => {
              alert('Company deleted successfully');
              this.router.navigate(['/companies']);
            },
            error: () => {
              alert('Failed to delete company');
            }
          });
      }
    });
  }


  addProduct(): void {
    const dialogRef = this.dialog.open(AddProductComponent, {
      width: '400px',
      data: { companyId: this.company.Id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.getCompany(this.company.Id,false);
      }
    });
  }

  removeProduct(productId: number): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: { 
        title: 'Remove Product', 
        message: 'Are you sure you want to remove this product from the company?' 
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.productService.deleteProduct( productId )
          .subscribe({
            next: (result) => {
              alert('Product removed');
              this.getCompany(this.company.Id,false);
            },
            error: () => {
              alert('Failed to remove product');
            }
          });
      }
    });
  }

  
  updateProduct(productId: number): void {
    this.productService.getProduct(productId, false).subscribe((product) => {
      const dialogRef = this.dialog.open(EditProductComponent, {
        width: '350px',
        data: {
          product,
          workers: this.company.Workers, 
        },
      });

      dialogRef.afterClosed().subscribe((updatedProduct) => {
        if (updatedProduct) {
          this.productService.updateProduct(productId, updatedProduct).subscribe({
            next: () => {
              alert('Product updated successfully');
              this.getCompany(this.company.Id,false);
            },
            error: () => {
              alert('Failed to update product');
            },
          });
        }
      });
    });
  }

}
