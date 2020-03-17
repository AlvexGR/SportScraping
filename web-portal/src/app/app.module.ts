import { BaseApiHelper } from './helpers/base-api.helper';
import { SportService } from './services/sport-service/sport.service';
import { ScrapingService } from './services/scraping-service/scraping.service';
import { MetricService } from './services/metric-service/metric.service';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from "@angular/common/http";
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { FilterComponent } from './components/filter/filter.component';
import { DataExportComponent } from './components/data-export/data-export.component';
import { ProgressComponent } from './components/progress/progress.component';

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    FilterComponent,
    DataExportComponent,
    ProgressComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
  ],
  providers: [
    BaseApiHelper,
    MetricService,
    ScrapingService,
    SportService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
