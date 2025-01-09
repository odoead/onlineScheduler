import { Component } from '@angular/core';
import { CompanyMin } from '../../global/models/companyMin';
import { CompanyService } from '../../global/services/company.service';
import { Router } from '@angular/router';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-company-list',
  standalone: true,
  imports: [MatListModule],
  templateUrl: './company-list.component.html',
  styleUrl: './company-list.component.css',
})
export class CompanyListComponent {
  companies: CompanyMin[] = [];

  constructor(private companyService: CompanyService, private router: Router) {}

  ngOnInit(): void {
    this.companyService.getCompaniesMin().subscribe({
      next: (result) => {
        this.companies = result;
      }
    });
  }

  redirectToCompany(companyId: number): void {
    this.router.navigate([`/company/${companyId}`]);
  }
}
