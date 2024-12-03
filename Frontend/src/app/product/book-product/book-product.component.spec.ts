import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BookProductComponent } from './book-product.component';

describe('BookProductComponent', () => {
  let component: BookProductComponent;
  let fixture: ComponentFixture<BookProductComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookProductComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BookProductComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
