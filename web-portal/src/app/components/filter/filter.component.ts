import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import Sport from 'src/app/models/sport';
import { SportService } from 'src/app/services/sport-service/sport.service';
import * as _ from "lodash";
import { ScrapeType } from 'src/app/models/provider';
import { Filter } from 'src/app/models/filter.model';

@Component({
  selector: 'app-filter',
  templateUrl: './filter.component.html'
})
export class FilterComponent implements OnInit {
  sports: Array<Sport>;
  selectedType: ScrapeType;
  selectedSport: string;

  @Output() filter = new EventEmitter<Filter>();
  constructor(private _sportService: SportService) { }

  async ngOnInit() {
    console.log("Getting all sports");
    this.sports = await this._sportService.getAll();
    if (this.sports && this.sports.length != 0) {
      this.selectedSport = _.first(this.sports).code;
      this.selectedType = ScrapeType.All;
      this.updateSelected(this.selectedSport, this.selectedType);
    } else {
      throw new Error("Sports are empty");
    }
  }

  updateSelected(sportCode?: string, scrapeType?: ScrapeType) {
    const filter = new Filter();
    filter.sportCode = sportCode;
    filter.scrapeType = scrapeType;
    this.filter.emit(filter);
  }

  handlSportSelect(sportCode: string) {
    this.selectedSport = sportCode;
    this.updateSelected(this.selectedSport, this.selectedType);
  }

  handleTypeClick(type: ScrapeType) {
    this.selectedType = type;
    this.updateSelected(this.selectedSport, this.selectedType);
  }
}
