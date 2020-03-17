import { Component, OnInit, Input } from "@angular/core";
import { ScrapingService } from "src/app/services/scraping-service/scraping.service";
import { Filter } from "src/app/models/filter.model";
import ScrapingInformation, {
  ScrapeStatus
} from "src/app/models/scraping-information";
import * as _ from "lodash";
import { ScrapeType } from "src/app/models/provider";
import * as moment from "moment";
import { Constant } from "src/app/helpers/constants.helper";

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html"
})
export class DashboardComponent implements OnInit {
  scrapeInfos: Array<ScrapingInformation>;
  constructor(private _scrapingService: ScrapingService) {}

  ngOnInit() {}

  public async loadScrapingInfos(filter: Filter) {
    if (!filter) {
      console.log("Filter is null");
      return;
    }
    this.scrapeInfos = await this._scrapingService.GetTodayScrapingInfo(
      filter.sportCode,
      filter.scrapeType
    );
    this.convertScrapeType();
    this.convertScrapeStatus();
    this.convertScrapeTime();
    this.scrapeInfos = _.reverse(
      _.sortBy(this.scrapeInfos, scrapeInfo => scrapeInfo.id)
    );
  }

  private convertScrapeType() {
    if (!this.scrapeInfos || this.scrapeInfos.length == 0) {
      console.log("Scrape infos is empty");
      return;
    }
    this.scrapeInfos.forEach(scrapeInfo => {
      if (!scrapeInfo.provider) {
        throw new Error(`Provider is empty for scrapeInfo: ${scrapeInfo.id}`);
      }
      switch (scrapeInfo.provider.scrapeType) {
        case ScrapeType.Competition:
          scrapeInfo.scrapeTypeDisplay = "Competition";
          break;
        case ScrapeType.PlayerHeadToHead:
          scrapeInfo.scrapeTypeDisplay = "Head to head";
          break;
        case ScrapeType.PlayerOverUnder:
          scrapeInfo.scrapeTypeDisplay = "Over / Under";
          break;
        default:
          console.log("Wrong scrape type: " + scrapeInfo.provider.scrapeType);
          break;
      }
    });
  }

  private convertScrapeStatus() {
    if (!this.scrapeInfos || this.scrapeInfos.length == 0) {
      console.log("Scrape infos is empty");
      return;
    }
    this.scrapeInfos.forEach(scrapeInfo => {
      switch (scrapeInfo.scrapeStatus) {
        case ScrapeStatus.Done:
          scrapeInfo.scrapeStatusDisplay = "Done";
          break;
        case ScrapeStatus.Failed:
          scrapeInfo.scrapeStatusDisplay = "Failed";
          break;
        case ScrapeStatus.Pending:
          scrapeInfo.scrapeStatusDisplay = "Pending";
          break;
        case ScrapeStatus.InProgress:
          scrapeInfo.scrapeStatusDisplay = "In progress";
          break;
        default:
          console.log("Wrong scrape status: " + scrapeInfo.provider.scrapeType);
          break;
      }
    });
  }

  private convertScrapeTime() {
    if (!this.scrapeInfos || this.scrapeInfos.length == 0) {
      console.log("Scrape infos is empty");
      return;
    }
    this.scrapeInfos.forEach(
      scrapeInfo =>
        (scrapeInfo.scrapeTimeDisplay = moment(scrapeInfo.scrapeTime).format(
          Constant.APP_DATE_FORMAT
        ))
    );
  }
}
