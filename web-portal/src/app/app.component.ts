import { Component, OnInit, ViewChild } from "@angular/core";
import { DashboardComponent } from "./components/dashboard/dashboard.component";
import { Filter } from "./models/filter.model";

@Component({
  selector: "app-root",
  templateUrl: "./app.component.html"
})
export class AppComponent implements OnInit {
  currentFilter: Filter;

  @ViewChild(DashboardComponent, { static: false })
  private _dashboardComponent: DashboardComponent;

  ngOnInit() {
    this.startInterval();
  }

  isSameFilter(filter: Filter): boolean {
    if (!this.currentFilter) {
      // not the same if currentFilter is null
      return false;
    }
    const result =
      filter.sportCode == this.currentFilter.sportCode &&
      filter.scrapeType == filter.scrapeType;
    return result;
  }

  async updateFilter(filter: Filter): Promise<void> {
    if (!filter) return;
    this.currentFilter = filter;
    await this._dashboardComponent.loadScrapingInfos(this.currentFilter);
  }

  startInterval() {
    // Fetch data every 3 seconds
    setInterval(async () => {
      await this._dashboardComponent.loadScrapingInfos(this.currentFilter);
    }, 3000);
  }
}
