﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VocaDb.Model.Domain.Albums;
using VocaDb.Model.Domain.Artists;
using VocaDb.Model.Domain.Globalization;
using VocaDb.Model.Helpers;

namespace VocaDb.Tests.Helpers {

	[TestClass]
	public class ArtistHelperTests {

		private IArtistWithSupport animator;
		private IArtistWithSupport circle;
		private IArtistWithSupport producer;
		private IArtistWithSupport producer2;
		private IArtistWithSupport producer3;
		private IArtistWithSupport producer4;
		private IArtistWithSupport vocalist;
		private IArtistWithSupport vocalist2;
		private IArtistWithSupport vocalist3;
		private IArtistWithSupport vocalist4;

		private IArtistWithSupport CreateArtist(ArtistType artistType, string name) {

			var p = new Artist { ArtistType = artistType };
			p.Names.Add(new ArtistName(p, new LocalizedString(name, ContentLanguageSelection.English)));
			return p.AddAlbum(new Album());

		}

		private TranslatedStringWithDefault CreateString(TranslatedString translatedString) {
			return new TranslatedStringWithDefault(translatedString.Japanese, translatedString.Romaji, translatedString.English, translatedString.Default ?? string.Empty);
		}

		private TranslatedStringWithDefault CreateString(string uniform) {
			return CreateString(TranslatedString.Create(uniform));
		}

		private string GetNames(params IArtistWithSupport[] artists) {
			return string.Join(", ", artists.Select(a => a.Artist.DefaultName));
		}

		[TestInitialize]
		public void SetUp() {

			animator = CreateArtist(ArtistType.Animator, "wakamuraP");
			circle = CreateArtist(ArtistType.Circle, "S.C.X.");
			producer = CreateArtist(ArtistType.Producer, "devilishP");
			producer2 = CreateArtist(ArtistType.Producer, "40mP");
			producer3 = CreateArtist(ArtistType.Producer, "Clean Tears");
			producer4 = CreateArtist(ArtistType.Producer, "Tripshots");
			vocalist = CreateArtist(ArtistType.Vocaloid, "Hatsune Miku");
			vocalist2 = CreateArtist(ArtistType.Vocaloid, "Kagamine Rin");
			vocalist3 = CreateArtist(ArtistType.Vocaloid, "Kagamine Len");
			vocalist4 = CreateArtist(ArtistType.Vocaloid, "Megurine Luka");

		}

		[TestMethod]
		public void GetCanonizedName_NotPName() {

			var result = ArtistHelper.GetCanonizedName("devilish5150");

			Assert.AreEqual("devilish5150", result, "result");

		}

		[TestMethod]
		public void GetCanonizedName_PName() {

			var result = ArtistHelper.GetCanonizedName("devilishP");

			Assert.AreEqual("devilish", result, "result");

		}

		[TestMethod]
		public void GetCanonizedName_PDashName() {

			var result = ArtistHelper.GetCanonizedName("devilish-P");

			Assert.AreEqual("devilish", result, "result");

		}

		[TestMethod]
		public void GetArtistString_Empty() {

			var result = ArtistHelper.GetArtistString(new IArtistWithSupport[] { }, false);

			Assert.AreEqual(CreateString(TranslatedString.Create(string.Empty)), result, "result is empty");

		}

		[TestMethod]
		public void GetArtistString_OneProducer() {

			var result = ArtistHelper.GetArtistString(new[] { producer }, false);

			Assert.AreEqual(CreateString(producer.Artist.Names.SortNames), result, "producer's name");

		}

		[TestMethod]
		public void GetArtistString_TwoProducers() {

			var result = ArtistHelper.GetArtistString(new[] { producer, producer2 }, false);

			Assert.AreEqual(CreateString(GetNames(producer, producer2)), result, "artist string has both producers");

		}

		[TestMethod]
		public void GetArtistString_OneProducerAndVocalist() {

			var result = ArtistHelper.GetArtistString(new[] { producer, vocalist }, false);

			Assert.AreEqual(CreateString(TranslatedString.Create(string.Format("{0} feat. {1}", producer.Artist.DefaultName, vocalist.Artist.DefaultName))), result, "artist string has producer and vocalist name");

		}

		[TestMethod]
		public void GetArtistString_MultipleProducersAndTwoVocalists() {

			// 3 producers and 2 vocalists
			var result = ArtistHelper.GetArtistString(new[] { producer, producer2, producer3, vocalist, vocalist2 }, false);

			Assert.AreEqual(string.Format("{0} feat. {1}", GetNames(producer, producer2, producer3), GetNames(vocalist, vocalist2)), result.Default, "artist string has multiple producers and vocalists");

		}

		[TestMethod]
		public void GetArtistString_OneProducerAndVariousVocalists() {

			var result = ArtistHelper.GetArtistString(new[] { producer, vocalist, vocalist2, vocalist3, vocalist4 }, false);

			Assert.AreEqual(CreateString(TranslatedString.Create(string.Format("{0} feat. various", producer.Artist.DefaultName))), result, "artist string has producer and various");

		}

		[TestMethod]
		public void GetArtistString_VariousArtists() {

			// 4 producers and 2 vocalists, various artists because of >= 4 producers
			var result = ArtistHelper.GetArtistString(new[] { producer, producer2, producer3, producer4, vocalist, vocalist2 }, false);

			Assert.AreEqual(CreateString(ArtistHelper.VariousArtists), result, "artist string is various artists");

		}

		/// <summary>
		/// One producer and circle, producer is shown first.
		/// </summary>
		[TestMethod]
		public void GetArtistString_OneProducerAndCircle_ProducerFirst() {

			var result = ArtistHelper.GetArtistString(new[] {circle, producer}, false);

			Assert.AreEqual(CreateString(TranslatedString.Create(producer.Artist.DefaultName + ", " + circle.Artist.DefaultName)), result, "Producer is shown first");

		}

		[TestMethod]
		public void GetArtistString_OnlyVocalist() {

			var result = ArtistHelper.GetArtistString(new[] { vocalist }, false);

			Assert.AreEqual(CreateString(vocalist.Artist.Names.SortNames), result, "artist string has vocalist name");

		}

		[TestMethod]
		public void GetArtistString_OneProducerAndAnimator_NotVideo() {

			var result = ArtistHelper.GetArtistString(new[] { producer, animator }, false);

			Assert.AreEqual(CreateString(producer.Artist.Names.SortNames), result, "artist string has one producer");

		}

		/// <summary>
		/// One producer and animator, the disc is video. Animator is shown first.
		/// </summary>
		[TestMethod]
		public void GetArtistString_OneProducerAndAnimator_IsVideo() {

			var result = ArtistHelper.GetArtistString(new[] { producer, animator }, true);

			Assert.AreEqual(CreateString(TranslatedString.Create(animator.Artist.DefaultName + ", " + producer.Artist.DefaultName)), result, "artist string has one producer and animator");

		}

		[TestMethod]
		public void GetMainCircle_HasCircle() {

			producer.Artist.AddGroup(circle.Artist);
			producer2.Artist.AddGroup(circle.Artist);

			var result = ArtistHelper.GetMainCircle(new[] { producer, producer2, circle }, false);

			Assert.AreEqual(circle.Artist, result, "Circle was returned");

		}

		[TestMethod]
		public void GetMainCircle_OnlyCircle() {

			var result = ArtistHelper.GetMainCircle(new[] { circle }, false);

			Assert.AreEqual(circle.Artist, result, "Circle was returned");

		}

		[TestMethod]
		public void GetMainCircle_ProducerNotInCircle() {

			producer.Artist.AddGroup(circle.Artist);

			var result = ArtistHelper.GetMainCircle(new[] { producer, producer2, circle }, false);

			Assert.IsNull(result, "No common circle found");

		}

		[TestMethod]
		public void GetMainCircle_NoCircle() {

			producer.Artist.AddGroup(circle.Artist);
			producer2.Artist.AddGroup(circle.Artist);

			var result = ArtistHelper.GetMainCircle(new[] { producer, producer2 }, false);

			Assert.IsNull(result, "No circle found");

		}

	}

}
