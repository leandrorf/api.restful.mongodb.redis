import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { HomeRoutingModule } from './home-routing.module';
import { HomeComponent } from './home.component';
import { HomeService } from './services/home.service';
import { FormsModule } from '@angular/forms';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { VideoComponent } from './video/video.component';
import { NewsComponent } from './news/news.component';
import { HeaderComponent } from '../components/header/header.component';
import { GalleryComponent } from './gallery/gallery.component';


@NgModule({
  declarations: [
    HomeComponent,
    VideoComponent,
    NewsComponent,
    HeaderComponent,
    GalleryComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    HomeRoutingModule,
    CKEditorModule
  ],
  providers: [
    HomeService
  ],
  exports: [
    HomeComponent
  ]
})
export class HomeModule { }
