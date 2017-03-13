import { Component, Input, AfterContentInit } from "@angular/core";
import { ITabComponentValue } from "../TabComponentValue";

@Component({
    selector: "tab",
    styles: [`
    .pane{
      padding: 1em;
    }
  `],
    template: require("./tab.component.html")
})
export class TabComponent implements AfterContentInit {
    ngAfterContentInit(): void {
        
    }

    @Input("tabTitle") title: string;
    @Input() active = false;

    public activate(value: boolean) {
        this.active = value;
    }
}
