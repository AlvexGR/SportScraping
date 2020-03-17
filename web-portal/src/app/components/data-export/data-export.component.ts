import { MetricService } from './../../services/metric-service/metric.service';
import { Component, OnInit, Input } from '@angular/core';
import * as moment from 'moment';

@Component({
  selector: 'app-data-export',
  templateUrl: './data-export.component.html'
})
export class DataExportComponent implements OnInit {
  @Input() sportCode: string;
  fetchingData: boolean;
  error: string;

  constructor(private _metricService: MetricService) { }

  ngOnInit() {
  }

  async exportExcelFile(countryCode: string) {
    this.error = null;
    this.fetchingData = true;
    try {
      const today = new Date();
      const data = await this._metricService.exportMetricData(countryCode, this.sportCode, today);
      if (!data) {
        console.log("No data to export");
        return;
      }
      const blob = new Blob([data], { type: '"application/excel";' });
      let fileName = `${this.sportCode.split('_')[0]}_${countryCode}_${moment(today).format('DDMMYYYY')}.xlsx`;
      const downloadUrl = URL.createObjectURL(blob);
      const a: HTMLAnchorElement = document.createElement('a') as HTMLAnchorElement;
      a.href = downloadUrl;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(downloadUrl);
    } catch (err) {
      this.error = "Something went wrong. Please try again later."
    }
    this.fetchingData = false;
  }
}
