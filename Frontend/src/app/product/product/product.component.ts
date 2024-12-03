import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ProductService } from '../../global/services/product.service';
import { Product } from '../../global/models/product';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product.component.html',
  styleUrl: './product.component.css',
})
export class ProductComponent implements OnInit {
  product!: Product;

  constructor(
    private dialog: MatDialog,
    private router: Router,
    private route: ActivatedRoute,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const companyId = +!params.get('id');
      this.getProduct(companyId, true);
    });
  }

  getProduct(id: number, isPageLoad: boolean) {
    this.productService.getProduct(id, isPageLoad).subscribe({
      next: (result) => {
        this.product = result;
      },
    });
  }

  bookWorker(workerId: string): void {//TODO
    console.log(`Booking worker ID: ${workerId}`);
  }

  redirectToWorker(workerId: string): void {
    this.router.navigate(['/worker', workerId]);
  } 

  redirectToCompany(id:number)
  {
    this.router.navigate(['/company', id]);
  }

  redirectToHome()
  {
    this.router.navigate(['/home']);
  }

  
}
