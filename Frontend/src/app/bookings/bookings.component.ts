import { Component, OnInit } from '@angular/core';
import { GetBookings } from '../global/models/getBooking';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { BookingService } from '../global/services/booking.service';
import { ConfirmDialogComponent } from '../global/confirm-dialog/confirm-dialog.component';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [MatCardModule,MatTableModule,CommonModule],
  templateUrl: './bookings.component.html',
  styleUrl: './bookings.component.css'
})
export class BookingsComponent implements OnInit {
  bookings: GetBookings[] = [];

  constructor(
    private dialog: MatDialog,
    private router: Router,
    private bookingService:BookingService
  ) {}

  ngOnInit(): void {
    this.bookingService.getBookings(true).subscribe((data) => (this.bookings = data));
  }

  confirmCancel(booking: GetBookings, action: 'confirm' | 'cancel' ) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '350px',
      data: {
        title: `Confirm ${action}`,
        message: `Are you sure you want to ${action} this booking?`,
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        if (action === 'confirm') {
          this.bookingService.changeBookingStatus(booking.id, BookingStatus.CONFIRMED);
        } else if (action === 'cancel') {
          this.bookingService.changeBookingStatus(booking.id, BookingStatus.CANCELED);
      }}
      this.bookingService.getBookings(true).subscribe((data) => (this.bookings = data));
    });
  }

  //TODO
  /*
  editBooking(bookingId: number): void {
    this.bookingService.getBookings(true).subscribe((data) => {
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
  }*/
}
