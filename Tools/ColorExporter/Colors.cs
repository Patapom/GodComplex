using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using SharpMath;

namespace CSharpColorExporter
{
	static class Colors {

		public static string	ParseColor( string _color ) {
			string	name, hexRGB;
			ReadNameHexRGB( _color, out name, out hexRGB );
			return ", " + name + "( \"" + hexRGB + "\" )";
		}

		public static void		ReadNameHexRGB( string _color, out string _name, out string _hexRGB ) {
			string[]	items = _color.Split( '\t' );
			_name = RemoveParentheses( items[0].ToLower() );
			foreach ( char C in _name )
				if ( !((C >= 'a' && C <= 'z') || (C >= 'A' && C <= 'Z') || (C >= '0' && C <= '9')) ) {
					_name = "\"" + _name + "\"";	// Quote the name if it has some unrecognized characters
					break;
				}

			if ( items[2] != "#" )
				throw new Exception( "Unexpected item!" );

			string		R = items[3];
			string		G = items[4];
			string		B = items[5];
			if ( R.Length != 2 || G.Length != 2 ||B.Length != 2 )
				throw new Exception( "Unexpected RGB item length!" );

			_hexRGB = "0x" + R + G + B;
		}

		static string	RemoveParentheses( string _name ) {
			int	start;
			while ( (start = _name.IndexOf( '(' )) != -1 ) {
				int	end = _name.IndexOf( ')', start );
				if ( end == -1 )
					throw new Exception( "End parenthesis not found!" );
				_name = _name.Remove( start, end+1-start );
			}

			_name = _name.Trim( ' ' );

			return _name;
		}

		public static float3	GetChromaLuma( string _color ) {
			string	name, hexRGB;
			ReadNameHexRGB( _color, out name, out hexRGB );
			return HexRGB2xyY( hexRGB );
		}

		static ImageUtility.ColorProfile	ms_profile = new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB );

		/// <summary>
		/// Converts a 0xRRGGBB string into 
		/// </summary>
		/// <param name="_hexRGB"></param>
		/// <returns></returns>
		public static float3	HexRGB2xyY( string _hexRGB ) {
			if ( _hexRGB.StartsWith( "0x" ) )
				_hexRGB = _hexRGB.Remove( 0, 2 );

			uint	V = uint.Parse( _hexRGB, System.Globalization.NumberStyles.HexNumber );
			byte	R = (byte) (V >> 16);
			byte	G = (byte) ((V >> 8) & 0xFF);
			byte	B = (byte) (V & 0xFF);

			float4	RGBA = new float4( R / 255.0f, G / 255.0f, B / 255.0f, 1.0f );
			float4	XYZ = new float4();
			ms_profile.RGB2XYZ( RGBA, ref XYZ );

			float3	xyY = new float3();
			ImageUtility.ColorProfile.XYZ2xyY( XYZ.xyz, ref xyY );

			return xyY;
		}

// From https://fr.wikipedia.org/wiki/Liste_de_noms_de_couleur
public static string	colors = 
@"Abricot	 	#	E6	7E	30	230	126	48	5	60	87	0	26	79	90\n\
Acajou	 	#	88	42	1D	136	66	29	0	51	79	47	21	79	53\n\
Aigue-marine	 	#	79	F8	F8	121	248	248	51	0	0	3	180	51	97\n\
Alezan (chevaux)	 	#	A7	67	26	167	103	38	0	38	77	35	30	77	65\n\
Amande	 	#	82	C4	6C	130	196	108	34	0	45	23	105	45	77\n\
Amarante	 	#	91	28	3B	145	40	59	0	72	59	43	349	72	57\n\
Ambre	 	#	F0	C3	00	240	195	0	0	19	100	6	49	100	94\n\
Améthyste	 	#	88	4D	A7	136	77	167	19	54	0	35	279	54	65\n\
Anthracite	 	#	30	30	30	48	48	48	0	0	0	81	0	0	19\n\
Aquilain (chevaux)	 	#	AD	4F	09	173	79	9	0	54	95	32	26	95	68\n\
Argent	 	#	C0	C0	C0	192	192	192	0	0	75	3	0	0	75\n\
Aubergine	 	#	37	00	28	55	0	40	0	100	27	78	316	100	22\n\
Auburn (cheveux)	 	#	9D	3E	0C	157	62	12	0	61	92	38	21	92	62\n\
Aurore	 	#	FF	CB	60	255	203	96	0	20	62	0	40	62	100\n\
Avocat	 	#	56	82	03	86	130	3	34	0	98	49	81	98	51\n\
Azur	 	#	00	7F	FF	0	127	255	100	50	0	0	210	100	100\n\
Baillet (chevaux, vieilli)	 	#	AE	64	2D	174	100	45	0	43	74	32	26	59	43\n\
Basané (teint)	 	#	8B	6C	42	139	108	66	0	22	53	45	35	36	40\n\
Beurre	 	#	F0	E3	6B	240	227	107	0	5	55	6	54	82	68\n\
Bis	 	#	76	6F	64	118	111	100	0	6	15	54	37	8	43\n\
Bisque	 	#	FF	E4	C4	255	228	196	0	11	23	0	33	100	88\n\
Bistre	 	#	85	6D	4D	133	109	77	0	18	42	48	34	27	41\n\
Bitume (pigment)	 	#	4E	3D	28	78	61	40	0	22	49	69	33	32	23\n\
Blanc cassé	 	#	FE	FE	E2	254	254	226	0	0	11	0	60	93	94\n\
Blanc lunaire	 	#	F4	FE	FE	244	254	254	4	0	0	0	180	83	98\n\
Blé	 	#	E8	D6	30	232	214	48	0	8	79	9	54	80	55\n\
Bleu acier	 	#	3A	8E	BA	58	142	186	69	24	0	27	201	52	48\n\
Bleu barbeau ou bleuet	 	#	54	72	AE	84	114	174	52	34	0	32	220	36	51\n\
Bleu canard	 	#	04	8B	9A	4	139	154	97	10	0	40	186	95	31\n\
Bleu céleste	 	#	26	C4	EC	38	196	236	84	17	0	7	192	84	54\n\
Bleu charrette	 	#	8E	A2	C6	142	162	198	28	18	0	22	219	33	67\n\
Bleu ciel	 	#	77	B5	FE	119	181	254	53	29	0	0	212	99	73\n\
Bleu de cobalt	 	#	22	42	7C	34	66	124	73	47	0	51	219	57	31\n\
Bleu de Prusse, de Berlin ou bleu hussard	 	#	24	44	5C	36	68	92	61	26	0	64	206	44	25\n\
Bleu électrique	 	#	2C	75	FF	44	117	255	83	54	0	0	219	100	59\n\
Bleu givré	 	#	80	D0	D0	128	208	208	38	0	0	18	180	46	66\n\
Bleu marine	 	#	03	22	4C	3	34	76	96	55	0	70	215	92	15\n\
Bleu nuit	 	#	0F	05	6B	15	5	107	86	95	0	58	246	91	22\n\
Bleu outremer	 	#	1B	01	9B	27	1	155	83	99	0	39	250	99	31\n\
Bleu paon	 	#	06	77	90	6	119	144	96	17	0	44	191	92	29\n\
Bleu persan	 	#	66	00	FF	102	0	255	60	100	0	0	264	100	50\n\
Bleu pétrole	 	#	1D	48	51	29	72	81	64	11	0	68	190	47	22\n\
Bleu roi ou de France	 	#	31	8C	E7	49	140	231	79	39	0	9	210	79	55\n\
Bleu turquin	 	#	42	5B	8A	66	91	138	52	34	0	46	219	35	40\n\
Blond vénitien (cheveux)	 	#	E7	A8	54	231	168	84	0	27	64	9	34	75	62\n\
Blond (cheveux)	 	#	E2	BC	74	226	188	116	0	17	49	11	39	65	67\n\
Bouton d'or	 	#	FC	DC	12	252	220	18	0	13	93	1	52	98	53\n\
Brique	 	#	84	2E	1B	132	46	27	0	65	80	48	11	66	31\n\
Bronze	 	#	61	4E	1A	97	78	26	0	20	73	62	44	58	24\n\
Brou de noix	 	#	3F	22	04	63	34	4	0	46	94	75	31	88	13\n\
Caca d'oie	 	#	CD	CD	0D	205	205	13	0	0	94	20	60	88	43\n\
Cacao	 	#	61	4B	3A	97	75	58	0	23	40	62	26	25	30\n\
Cachou (pigments)	 	#	2F	1B	0C	47	27	12	0	43	74	82	26	59	12\n\
Cæruleum	 	#	35	7A	B7	53	122	183	71	33	0	28	208	55	46\n\
Café	 	#	46	2E	01	70	46	1	0	34	99	73	39	97	14\n\
Café au lait	 	#	78	5E	2F	120	94	47	0	22	61	53	39	44	33\n\
Cannelle	 	#	7E	58	35	126	88	53	0	30	58	51	29	41	35\n\
Capucine	 	#	FF	5E	4D	255	94	77	0	63	70	0	6	100	65\n\
Caramel (pigments)	 	#	7E	33	00	126	51	0	0	60	100	51	24	100	25\n\
Carmin (pigment)	 	#	96	00	18	150	0	24	0	100	84	41	350	100	29\n\
Carotte	 	#	F4	66	1B	244	102	27	0	58	89	4	21	91	53\n\
Chamois	 	#	D0	C0	7A	208	192	122	0	8	41	18	49	48	65\n\
Chartreuse	 	#	7F	FF	00	127	255	0	50	0	100	0	90	100	50\n\
Châtain (cheveux)	 	#	8B	6C	42	139	108	66	0	22	53	45	35	36	40\n\
Chaudron	 	#	85	53	0F	133	83	15	0	38	89	48	35	80	29\n\
Chocolat	 	#	5A	3A	22	90	58	34	0	36	62	65	26	45	24\n\
Cinabre (pigment)	 	#	DB	17	02	219	23	2	0	89	99	14	6	98	43\n\
Citrouille	 	#	DF	6D	14	223	109	20	0	51	91	13	26	84	48\n\
Coquille d'œuf	 	#	FD	E9	E0	253	233	224	0	8	11	1	19	88	94\n\
Corail	 	#	E7	3E	01	231	62	1	0	73	100	9	16	99	45\n\
Cramoisi	 	#	DC	14	3C	220	20	60	0	91	73	14	348	83	47\n\
Cuisse de nymphe	 	#	FE	E7	F0	254	231	240	0	9	6	0	337	92	95\n\
Cuivre	 	#	B3	67	00	179	103	0	0	42	100	30	35	100	35\n\
Cyan	 	#	2B	FA	FA	43	250	250	83	0	0	2	180	95	57\n\
Écarlate	 	#	ED	00	00	237	0	0	0	100	100	7	0	100	46\n\
Écru	 	#	FE	FE	E0	254	254	224	0	0	12	0	60	94	94\n\
Émeraude	 	#	01	D7	58	1	215	88	100	0	59	16	144	99	42\n\
Fauve	 	#	AD	4F	09	173	79	9	0	54	95	32	26	90	36\n\
Flave	 	#	E6	E6	97	230	230	151	0	0	34	10	60	61	75\n\
Fraise	 	#	BF	30	30	191	48	48	0	75	75	25	0	60	47\n\
Fraise écrasée	 	#	A4	24	24	164	36	36	0	78	78	36	0	64	39\n\
Framboise	 	#	C7	2C	48	199	44	72	0	78	64	22	349	64	48\n\
Fuchsia	 	#	FD	3F	92	253	63	146	0	75	42	1	334	75	62\n\
Fumée	 	#	BB	D2	E1	187	210	225	17	7	0	12	204	39	81\n\
Garance (pigment)	 	#	EE	10	10	238	16	16	0	93	93	7	0	87	50\n\
Glauque	 	#	64	9B	88	100	155	136	35	0	12	39	159	22	50\n\
Glycine	 	#	C9	A0	DC	201	160	220	9	27	0	14	281	46	75\n\
Grège	 	#	BB	AE	98	187	174	152	0	7	19	27	38	20	66\n\
Grenadine	 	#	E9	38	3F	233	56	63	0	76	73	9	358	80	57\n\
Grenat	 	#	6E	0B	14	110	11	20	0	90	82	57	355	82	24\n\
Gris	 	#	60	60	60	96	96	96	0	0	0	62	0	0	38\n\
Gris acier	 	#	AF	AF	AF	175	175	175	0	0	0	31	0	0	69\n\
Gris de Payne	 	#	67	71	79	103	113	121	15	7	0	53	207	8	44\n\
Gris fer	 	#	7F	7F	7F	127	127	127	0	0	0	50	0	0	50\n\
Gris perle	 	#	CE	CE	CE	206	206	206	0	0	0	19	0	0	81\n\
Gris souris	 	#	9E	9E	9E	158	158	158	0	0	0	38	0	0	62\n\
Groseille	 	#	CF	0A	1D	207	10	29	0	95	86	19	354	91	43\n\
Gueules (héraldique)	 	#	E2	13	13	226	19	19	0	92	92	11	0	84	48\n\
Héliotrope	 	#	DF	73	FF	223	115	255	13	55	0	0	286	100	73\n\
Incarnat	 	#	FF	6F	7D	255	111	125	0	56	51	0	354	100	72\n\
Indigo	 	#	79	1C	F8	121	28	248	89	51	0	3	265	94	54\n\
Indigo2 (teinture)	 	#	2E	00	6C	46	0	108	57	100	0	58	266	100	21\n\
Isabelle	 	#	78	5E	2F	120	94	47	0	22	61	53	39	44	33\n\
Jaune canari	 	#	E7	F0	0D	231	240	13	4	0	95	6	62	90	50\n\
Jaune citron	 	#	F7	FF	3C	247	255	60	3	0	76	0	62	100	62\n\
Jaune d'or	 	#	EF	D8	07	239	216	7	0	10	97	6	54	94	48\n\
Jaune de cobalt	 	#	FD	EE	00	253	238	0	0	6	100	0	56	90	99\n\
Jaune de Mars (pigment)	 	#	EE	D1	53	238	209	83	0	12	65	7	49	82	63\n\
Jaune de Naples (pigment)	 	#	FF	F0	BC	255	240	188	0	6	26	0	47	100	87\n\
Jaune impérial	 	#	FF	E4	36	255	228	54	0	11	79	0	52	100	61\n\
Jaune mimosa	 	#	FE	F8	6C	254	248	108	0	2	57	0	58	99	71\n\
Lapis-lazuli	 	#	26	61	9C	38	97	156	76	38	0	39	210	61	38\n\
Lavallière (reliure)	 	#	8F	59	22	143	89	34	0	38	76	44	30	62	35\n\
Lavande	 	#	96	83	EC	150	131	236	36	44	0	7	251	73	72\n\
Lie de vin	 	#	AC	1E	44	172	30	68	0	83	60	33	344	70	40\n\
Lilas	 	#	B6	66	D2	182	102	210	13	51	0	18	284	55	61\n\
Lime ou vert citron	 	#	9E	FD	38	158	253	56	38	0	78	1	89	98	61
Lin	 	#	FA	F0	E6	250	240	230	0	4	8	2	30	67	94\n\
Magenta	 	#	FF	00	FF	255	0	255	300	100	50	300	100	100	0\n\
Maïs	 	#	FF	DE	75	255	222	117	0	13	54	0	46	100	73\n\
Malachite	 	#	1F	A0	55	31	160	85	81	0	47	37	145	68	37\n\
Mandarine	 	#	FE	A3	47	254	163	71	0	36	72	0	30	99	64\n\
Mastic	 	#	B3	B1	91	179	177	145	0	1	19	30	56	18	64\n\
Mauve	 	#	D4	73	D4	212	115	212	0	46	0	17	300	53	64\n\
Menthe	 	#	16	B8	4E	22	184	78	88	0	58	28	141	79	40\n\
Moutarde	 	#	C7	CF	00	199	207	0	4	0	100	19	62	100	41\n\
Nacarat	 	#	FC	5D	5D	252	93	93	0	63	63	1	0	63	99\n\
Nankin	 	#	F7	E2	69	247	226	105	0	9	57	3	51	90	69\n\
Noisette	 	#	95	56	28	149	86	40	0	42	73	42	25	58	37\n\
Ocre jaune	 	#	DF	AF	2C	223	175	44	0	22	80	13	44	74	52\n\
Ocre rouge	 	#	DD	98	5C	221	152	92	0	31	58	13	28	65	61\n\
Olive	 	#	70	8D	23	112	141	35	21	0	75	45	76	60	35\n\
Or	 	#	FF	D7	00	255	215	0	0	16	100	0	51	100	50\n\
Orange brûlé	 	#	CC	55	00	204	85	0	0	58	100	20	25	100	40\n\
Orchidée	 	#	DA	70	D6	218	112	214	0	49	2	15	302	59	65\n\
Orpiment (pigment)	 	#	FC	D2	1C	252	210	28	0	17	89	1	49	97	55\n\
Paille	 	#	FE	E3	47	254	227	71	0	11	72	0	51	99	64\n\
Parme	 	#	CF	A0	E9	207	160	233	11	31	0	9	279	62	77\n\
Pelure d'oignon	 	#	D5	84	90	213	132	144	0	38	32	16	351	49	68\n\
Pervenche	 	#	CC	CC	FF	204	204	255	20	20	0	0	240	100	90\n\
Pistache	 	#	BE	F5	74	190	245	116	22	0	53	4	86	87	71\n\
Poil de chameau	 	#	B6	78	23	182	120	35	0	34	81	29	35	68	43\n\
Ponceau ou Coquelicot	 	#	C6	08	00	198	8	0	0	96	100	22	2	100	39\n\
Pourpre (héraldique)	 	#	9E	0E	40	158	14	64	0	91	59	38	339	84	34\n\
Prasin	 	#	4C	A6	6B	76	166	107	54	0	36	35	141	37	47\n\
Prune	 	#	81	14	53	129	20	83	0	84	36	49	325	73	29\n\
Puce	 	#	4E	16	09	78	22	9	0	72	88	69	11	79	17\n\
Rose Mountbatten	 	#	99	7A	8D	153	122	141	0	20	8	40	323	13	54\n\
Rouge anglais (pigment)	 	#	F7	23	0C	247	35	12	0	86	95	3	6	94	51\n\
Rouge cardinal	 	#	B8	20	10	184	32	16	0	83	91	28	6	84	39\n\
Rouge cerise	 	#	BB	0B	0B	187	11	11	0	94	94	27	0	89	39\n\
Rouge d'Andrinople	 	#	A9	11	01	169	17	1	0	90	99	34	6	99	33\n\
Rouge de Falun (pigment)	 	#	80	18	18	128	24	24	0	81	81	50	0	68	30\n\
Rouge feu	 	#	FF	49	01	255	73	1	0	71	100	0	17	100	50\n\
Rouge indien	 	#	CD	5C	5C	205	92	92	0	53	1	58	2	0	55\n\
Rouge sang	 	#	85	06	06	133	6	6	0	95	95	48	0	91	27\n\
Rouge tomette	 	#	AE	4A	34	174	74	52	0	57	70	32	11	54	44\n\
Rouille	 	#	98	57	17	152	87	23	0	43	85	40	30	74	34\n\
Roux	 	#	AD	4F	09	173	79	9	0	54	95	32	26	90	36\n\
Rubis	 	#	E0	11	5F	224	17	95	0	92	58	12	337	92	88\n\
Sable	 	#	E0	CD	A9	224	205	169	0	8	25	12	39	47	77\n\
Safre	 	#	01	31	B4	1	49	180	99	73	0	29	224	99	35\n\
Sang de bœuf	 	#	73	08	00	115	8	0	0	93	100	55	4	100	23\n\
Sanguine	 	#	85	06	06	133	6	6	0	95	95	48	0	91	27\n\
Saphir	 	#	01	31	B4	1	49	180	99	73	0	29	224	99	35\n\
Sarcelle	 	#	00	80	80	0	128	128	180	100	25	1	180	100	50\n\
Saumon	 	#	F8	8E	55	248	142	85	0	43	66	3	21	92	65\n\
Sépia	 	#	AE	89	64	174	137	100	0	21	43	32	30	31	54\n\
Sinople (héraldique)	 	#	14	94	14	20	148	20	86	0	86	42	120	76	33\n\
Smalt	 	#	00	33	99	0	51	153	100	67	0	40	220	100	30\n\
Smaragdin	 	#	01	D7	58	1	215	88	100	0	59	16	144	99	42\n\
Soufre	 	#	FF	FF	6B	255	255	107	0	0	58	0	60	100	71\n\
Tabac	 	#	9F	55	1E	159	85	30	0	47	81	38	26	68	37\n\
Taupe	 	#	46	3F	32	70	63	50	0	10	29	73	39	17	24\n\
Terre d'ombre	 	#	92	6D	27	146	109	39	0	25	73	43	39	58	36\n\
Tomate	 	#	DE	29	16	222	41	22	0	82	90	13	6	82	48\n\
Topaze	 	#	FA	EA	73	250	234	115	0	6	54	2	53	93	72\n\
Tourterelle ou Colombin	 	#	BB	AC	AC	187	172	172	0	8	8	27	0	10	70\n\
Turquoise	 	#	25	FD	E9	37	253	233	85	0	8	1	174	98	57\n\
Vanille	 	#	E1	CE	9A	225	206	154	0	8	32	12	44	54	74\n\
Vermeil	 	#	FF	09	21	255	9	33	0	96	87	0	354	100	52\n\
Vermillon	 	#	DB	17	02	219	23	2	0	89	99	14	6	98	43\n\
Vert bouteille	 	#	09	6A	09	9	106	9	92	0	92	58	120	84	23\n\
Vert céladon	 	#	83	A6	97	131	166	151	21	0	9	35	154	16	58\n\
Vert d'eau	 	#	B0	F2	B6	176	242	182	27	0	25	5	125	72	82\n\
Vert de chrome	 	#	18	39	1E	24	57	30	58	0	47	78	131	41	16\n\
Vert-de-gris	 	#	95	A5	95	149	165	149	10	0	10	35	120	8	62\n\
Vert de Hooker	 	#	1B	4F	08	27	79	8	66	0	90	69	104	82	17\n\
Vert de vessie	 	#	22	78	0F	34	120	15	72	0	88	53	109	78	26\n\
Vert épinard	 	#	17	57	32	23	87	50	74	0	43	66	145	58	22\n\
Vert impérial	 	#	00	56	1B	0	86	27	100	0	69	66	139	100	17\n\
Vert lichen	 	#	85	C1	7E	133	193	126	31	0	35	24	114	35	63\n\
Vert olive	 	#	55	6B	2F	85	107	47	82	39	30	2	82	56	1\n\
Vert perroquet	 	#	3A	F2	4B	58	242	75	76	0	69	5	126	88	59\n\
Vert poireau	 	#	4C	A6	6B	76	166	107	54	0	36	35	141	37	47\n\
Vert pomme	 	#	34	C9	24	52	201	36	74	0	82	21	114	70	46\n\
Vert prairie	 	#	57	D5	3B	87	213	59	59	0	72	16	109	65	53\n\
Vert printemps	 	#	00	FF	7F	0	255	127	100	0	50	0	150	100	50\n\
Vert sapin	 	#	09	52	28	9	82	40	89	0	51	68	145	80	18\n\
Vert sauge	 	#	68	9D	71	104	157	113	34	0	28	38	130	21	51\n\
Vert tilleul	 	#	A5	D1	52	165	209	82	21	0	61	18	81	58	57\n\
Vert Véronèse	 	#	5A	65	21	90	101	33	11	0	67	60	70	51	26\n\
Violet d'évêque	 	#	72	3E	64	114	62	100	0	46	12	55	316	30	35\n\
Violet foncé	 	#	94	00	D3	148	0	211	282	1	100	41	4	282	1\n\
Viride	 	#	40	82	6D	64	130	109	51	0	16	49	161	34	38\n\
Zinzolin	 	#	6C	02	77	108	2	119	9	98	0	53	294	97	24\n\";

	}
}
